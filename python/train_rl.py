import argparse
import os
import glob
from dataclasses import dataclass
from peaceful_pie.unity_comms import UnityComms
from stable_baselines3 import PPO
from stable_baselines3.common.monitor import Monitor
from stable_baselines3.common.callbacks import CheckpointCallback
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

    # Set up a checkpoint callback to save progress periodically
    checkpoint_callback = CheckpointCallback(
        save_freq=2048,  # Adjust this frequency as needed
        save_path="models",
        name_prefix="chess_agent"
    )

    # Train the agent with checkpoint saving
    ppo.learn(total_timesteps=100000, callback=checkpoint_callback)

    # Save the final model after training
    ppo.save("models/trained_chess_agent")
    print("Final model saved successfully at 'models/trained_chess_agent'.")

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)
