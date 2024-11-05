import argparse
from dataclasses import dataclass
from typing import Any, Tuple
import numpy as np
from numpy.typing import NDArray
from peaceful_pie.unity_comms import UnityComms
from gymnasium import Env, spaces
from stable_baselines3.common.env_checker import check_env

@dataclass
class Observation:
    board_state: NDArray[np.int8]
    selected_piece: int
    in_check: int
    curr_player: int

@dataclass
class RlResult:
    reward: float
    finished: bool
    obs: Observation

class MyEnv(Env):
    def __init__(self, unity_comms: UnityComms):
        self.unity_comms = unity_comms
        self.status = 0

        # Flattened observation space as a 1D Box
        self.observation_space = spaces.Box(
            low=-6, high=6, shape=(8 * 8 + 1 + 1 + 64 + 64 + 1,), dtype=np.float32
        )

        # Single Discrete action space (4096 possible actions)
        self.action_space = spaces.Discrete(64 * 64)

    def get_piece_mask(self, current_player: int) -> NDArray[np.int8]:
        mask = np.zeros(64, dtype=np.int8)
        for i in range(64):
            if self.unity_comms.isPlayerPiece(index=i, player=current_player) == True:
                mask[i] = 1
        return mask

    def get_move_mask(self, selected_piece: int) -> NDArray[np.int8]:
        mask = np.zeros(64, dtype=np.int8)
        for i in range(64):
            if self.unity_comms.isValidPieceMove(index=i, piece=selected_piece) == True:
                mask[i] = 1
        return mask
    
    def _get_observation(self, sel_piece: int) -> np.ndarray:
        # Get the current board state, player, and valid actions
        current_player = self.unity_comms.getCurrPlayer()
        piece_mask = self.get_piece_mask(current_player)
        move_mask = np.zeros(64, dtype=np.int8)  # Initialize empty move mask
        
        # If a piece is selected, update the move mask
        selected_piece = self.unity_comms.getSelectedPiece(index=sel_piece)
        if selected_piece:
            move_mask = self.get_move_mask(selected_piece)

        board_state = np.array(self.unity_comms.GetBoardMatrix()).flatten()
        
        # Compile full observation
        obs = np.concatenate([
            board_state,
            [selected_piece],
            [self.unity_comms.InCheck()],
            piece_mask,
            move_mask,
            [current_player]
        ]).astype(np.float32)

        return obs

    def step(self, action: int) -> Tuple[np.ndarray, float, bool, bool, dict[str, Any]]:
        if (self.unity_comms.getGameType() == 0 or self.unity_comms.getGameType() == 1 and self.unity_comms.getCurrPlayer() == 1):
            reward = 0
            terminated = False
            truncated = False
            info = {"finished": False}
            
            # Repeat the observation without changing the board state
            obs = self._get_observation(-1)
            return obs, reward, terminated, truncated, info

        self.status += 1

        if self.status % 10 == 0:
            print("TIMESTEP:", self.status)
        # Decode the action
        selected_piece = int(action // 64)
        selected_move = int(action % 64)
        
        current_player = self.unity_comms.getCurrPlayer()
        piece_mask = self.get_piece_mask(current_player)
        move_mask = self.get_move_mask(selected_piece)
        #print("(STEP) TURN:", current_player, "Selected Piece:", selected_piece, "Selected Move:", selected_move)
        if piece_mask[selected_piece] == 0 or move_mask[selected_move] == 0:
            # Invalid action - add a negative reward or end the step early if desired
            reward = -1.0
            terminated = False
            truncated = False
            info = {"finished": False, "invalid_action": True}
            
            # Repeat the observation without changing the board state
            obs = self._get_observation(selected_piece)
            return obs, reward, terminated, truncated, info
    
        # Communicate with Unity to take a step
        rl_result: RlResult = self.unity_comms.Step(piece_action=selected_piece, move_action=selected_move)
        
        # Convert board_state to a NumPy array if it's a list, then flatten
        board_state = np.array(rl_result["obs"]["board_state"]) if isinstance(rl_result["obs"]["board_state"], list) else rl_result["obs"]["board_state"]
        obs = np.concatenate([
            board_state.flatten(),
            [rl_result["obs"]["selected_piece"]],
            [rl_result["obs"]["in_check"]],
            piece_mask,
            move_mask,
            [rl_result["obs"]["curr_player"]]
        ]).astype(np.float32)

        reward = rl_result["reward"]
        terminated = rl_result["finished"]
        truncated = False
        info = {"finished": rl_result["finished"]}
        #print("Reward:", reward)
        return obs, reward, terminated, truncated, info
    
    def reset(self, seed=None, options=None) -> Tuple[np.ndarray, dict[str, Any]]:
        self.status = 0
        obs_vec3 = self.unity_comms.Reset(ResultClass=Observation)
        current_player = self.unity_comms.getCurrPlayer()

        # Convert board_state to a NumPy array if it's a list, then flatten
        board_state = np.array(obs_vec3.board_state) if isinstance(obs_vec3.board_state, list) else obs_vec3.board_state
        obs = np.concatenate([
            board_state.flatten(),
            [0],  # Initial selected piece index
            [obs_vec3.in_check],
            self.get_piece_mask(current_player),
            np.zeros(64, dtype=np.int8),  # Initialize move mask as zeros
            [1]
        ]).astype(np.float32)

        info = {"finished": False}
        return obs, info

def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    my_env = MyEnv(unity_comms=unity_comms)
    check_env(env=my_env)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)
