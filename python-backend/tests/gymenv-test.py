import numpy as np
import sys
from pathlib import Path

import gymnasium as gym
from gymnasium import Env

# append parent directory to search path for import
sys.path.append(str(Path(__file__).resolve().parent.parent))

from mlagents_envs.environment import UnityEnvironment, ActionTuple
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel
from unity_gym_env import UnityToGymWrapper

channel = EngineConfigurationChannel()
unity_env = UnityEnvironment(file_name=None, side_channels=[channel])
np.set_printoptions(precision=4, suppress=True)

# flatten_branched converts to a gym.Discrete action space
# instead of a gym.MultiDiscrete
env = UnityToGymWrapper(unity_env, flatten_branched=True)

observation, info = env.reset()

print(f"Starting observation: {observation}")


try:
    for episode in range(3):
        state, info = env.reset()
        step_count = 0
        episode_over = False
        total_reward = 0
        while not episode_over:
            action = env.action_space.sample()

            observation, reward, terminated, truncated, info = env.step(action)

            total_reward += reward
            episode_over = terminated or truncated
            step_count += 1

            if reward > 0.5:
                    print(f"Step {step_count}: Zombie hit detected. Reward: {reward}") 
                    # doesn't work when car speed goes fast enough
            elif reward < -2.0:
                print(f"Step {step_count}: Car hit detected. Reward: {reward}")
            elif reward < -0.5:
                print(f"Step {step_count}: Human hit detected Reward: {reward}")
except Exception as e:
    print(e)
finally:
    print(f"Episode finished. Total reward: {total_reward}")
    env.close()