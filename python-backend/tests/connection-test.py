import random
import numpy as np

from mlagents_envs.environment import UnityEnvironment, ActionTuple
from mlagents_envs.side_channel.engine_configuration_channel import EngineConfigurationChannel

# connect to unity editor (leave file_name empty if running press-play in Unity)
# could also pass path to complete exe build

channel = EngineConfigurationChannel()
env = UnityEnvironment(file_name=None, side_channels=[channel])
np.set_printoptions(precision=4, suppress=True)

try:
    env.reset()

    behavior_name = list(env.behavior_specs.keys())[0]
    spec = env.behavior_specs[behavior_name]

    # basic loop
    for episode in range(1000):
        env.reset()
        decision_steps, terminal_steps = env.get_steps(behavior_name)
        tracked_agent = decision_steps.agent_id[0]

        done = False
        while not done:
            # get current state
            state = decision_steps[tracked_agent].obs[0]
            # obs[0] is rays
            # obs 0 is one-hot encoded with 4 values for each
            #
            # obs[1] is custom scalar values
            print(decision_steps[tracked_agent].obs)

            # selection action
            action = random.randint(0, 8)

            # convert to ActionTuple format expected by ML-Agents
            action_tuple = ActionTuple()
            action_tuple.add_discrete(np.array([[action]]))

            env.set_actions(behavior_name, action_tuple)

            env.step()

            # get new environment state
            decision_steps, terminal_steps = env.get_steps(behavior_name)

            if tracked_agent in terminal_steps:
                # episode ended (EndEpisode() was called in Unity)
                reward = terminal_steps[tracked_agent].reward
                next_state = terminal_steps[tracked_agent].obs[0]
                done = True
            else:
                # normal step
                reward = decision_steps[tracked_agent].reward
                next_state = decision_steps[tracked_agent].obs[0]

            # store experience in replay buffer
            # dqn.push(state, action, reward, next_state, done)
    env.close()
except Exception as e:
    print(e)

finally:
    print("Closing Connection")
    env.close()