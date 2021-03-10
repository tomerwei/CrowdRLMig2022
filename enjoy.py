import time
import argparse

from coltra.agents import Agent, CAgent
from coltra.collectors import collect_crowd_data
from coltra.envs.unity_envs import UnitySimpleCrowdEnv, Mode

from typarse import BaseParser

class Parser(BaseParser):
    steps: int = 500
    env: str
    start_dir: str
    start_idx: int = -1
    wait: int = 1
    seed: int = 0
    mode: str = "random"
    num_agents: int = 20

    _help = {
        "steps": "Number of steps agent should collect in one episode",
        "env": "Path to the Unity environment binary",
        "start_dir": "Name of the tb directory containing the run from which we want to (re)start the coltra",
        "start_idx": "From which iteration we should start (only if start_dir is set)",
        "wait": "How many seconds to sleep before running",
        "seed": "Random seed for Unity",
        "mode": "Which board mode should be used",
        "num_agents": "How many agents should be in the environment"
    }

    _abbrev = {
        "steps": "s",
        "env": "e",
        "start_dir": "sd",
        "start_idx": "si",
        "wait": "w",
        "seed": "seed",
        "mode": "m",
        "num_agents": "na",
    }


if __name__ == '__main__':

    args = Parser()

    agent = CAgent.load_agent(args.start_dir, weight_idx=args.start_idx)

    time.sleep(args.wait)

    env = UnitySimpleCrowdEnv(args.env, seed=args.seed)
    env.engine_channel.set_configuration_parameters(width=1000, height=1000, time_scale=1)

    for _ in range(5):

        data, metrics = collect_crowd_data(agent, env, args.steps,
                                           mode=Mode.from_string(args.mode),
                                           num_agents=args.num_agents)
