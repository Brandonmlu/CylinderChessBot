import argparse
import os
import glob
from dataclasses import dataclass
from peaceful_pie.unity_comms import UnityComms
from stable_baselines3 import PPO
from stable_baselines3.common.monitor import Monitor
from stable_baselines3.common.evaluation import evaluate_policy
import numpy as np
from numpy.typing import NDArray
from my_env import MyEnv

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

def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    my_env = MyEnv(unity_comms=unity_comms)
    my_env = Monitor(my_env)

    # Check if there are existing checkpoints
    checkpoint_files = glob.glob("models/chess_agent_*.zip")
    if checkpoint_files:
        latest_checkpoint = max(checkpoint_files, key=os.path.getctime)
        print(f"Loading latest checkpoint: {latest_checkpoint}")
        ppo = PPO.load(latest_checkpoint, env=my_env)
    else:
        print("No checkpoint found, creating a new model...")
        ppo = PPO("MlpPolicy", env=my_env, verbose=1, ent_coef=0.1)


    mean_reward, std_reward = evaluate_policy(ppo, ppo.get_env(), n_eval_episodes=10)
    vec_env = ppo.get_env()
    obs = vec_env.reset()
    for i in range(1000):
        action, _states = ppo.predict(obs, deterministic=True)
        obs, rewards, dones, info = vec_env.step(action)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)
