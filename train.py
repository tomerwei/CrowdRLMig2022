from typing import Optional

import torch
import yaml
from typarse import BaseParser

from coltra.agents import CAgent
from coltra.envs.unity_envs import UnitySimpleCrowdEnv
from coltra.envs.probe_envs import ConstRewardEnv
from models.mlp_models import FancyMLPModel
from coltra.trainers import PPOCrowdTrainer


class Parser(BaseParser):
    config: str = ".coltra//configs/base_config.yaml"
    iters: int = 1000
    env: str
    name: str
    start_dir: Optional[str]
    start_idx: Optional[int] = -1

    _help = {
        "config": "Config file for the coltra",
        "iters": "Number of coltra iterations",
        "env": "Path to the Unity environment binary",
        "name": "Name of the tb directory to store the logs",
        "start_dir": "Name of the tb directory containing the run from which we want to (re)start the coltra",
        "start_idx": "From which iteration we should start (only if start_dir is set)",
    }

    _abbrev = {
        "config": "c",
        "iters": "i",
        "env": "e",
        "name": "n",
        "start_dir": "sd",
        "start_idx": "si",
    }


if __name__ == '__main__':
    CUDA = torch.cuda.is_available()

    args = Parser()

    with open(args.config, "r") as f:
        config = yaml.load(f.read(), yaml.Loader)

    trainer_config = config["trainer"]
    model_config = config["model"]

    trainer_config["tensorboard_name"] = args.name
    trainer_config["ppo_config"]["use_gpu"] = CUDA

    workers = trainer_config.get("workers") or 8  # default value

    # Initialize the environment
    if args.env == "probe":
        env = ConstRewardEnv(20)
    else:
        env = UnitySimpleCrowdEnv(args.env)
        env.engine_channel.set_configuration_parameters(time_scale=100)

    # Initialize the agent
    obs_size = next(iter(env.reset().values())).vector.shape[0]
    model_config["input_size"] = obs_size
    if args.start_dir:
        agent = CAgent.load_agent(args.start_dir, weight_idx=args.start_idx)
    else:
        model = FancyMLPModel(model_config)
        agent = CAgent(model)

    if CUDA:
        agent.cuda()

    # env = SubprocVecEnv([
    #     get_env_creator(file_name=args.env, no_graphics=True, worker_id=i, seed=i)
    #     for i in range(workers)
    # ])

    trainer = PPOCrowdTrainer(agent, env, config)
    trainer.train(args.iters, disable_tqdm=False, save_path=trainer.path)
