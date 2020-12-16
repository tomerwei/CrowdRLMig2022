from typing import Dict
import argparse

import torch

from agents import Agent
from models import BaseModel, MLPModel, FancyMLPModel
from trainers import PPOCrowdTrainer
import yaml


if __name__ == '__main__':
    CUDA = torch.cuda.is_available()

    parser = argparse.ArgumentParser(description='Read training arguments')
    parser.add_argument("--config", "-c", action="store", type=str, default="./configs/base_config.yaml",
                        help="Config file for the training")
    parser.add_argument("--iters", "-i", action="store", type=int, default=1000,
                        help="Number of training iterations")
    parser.add_argument("--env", "-e", action="store", type=str, default="Test.app",
                        help="Path to the Unity environment binary")
    parser.add_argument("--name", "-n", action="store", type=str, default=None,
                        help="Name of the tb directory to store the logs")
    parser.add_argument("--start_dir", "-sd", action="store", type=str, default=None,
                        help="Name of the tb directory containing the run from which we want to (re)start the training")
    parser.add_argument("--start_idx", "-si", action="store", type=int, default=-1,
                        help="From which iteration we should start (only if start_dir is set)")

    args = parser.parse_args()

    print(args)

    with open(args.config, "r") as f:
        config = yaml.load(f.read(), yaml.Loader)

    trainer_config = config["trainer"]
    model_config = config["model"]

    trainer_config["tensorboard_name"] = args.name
    trainer_config["use_gpu"] = CUDA

    action_range = None

    if args.start_dir:
        agent = Agent.load_agent(args.start_dir, action_range=action_range, weight_idx=args.start_idx)
    else:
        model = MLPModel(model_config)
        agent = Agent(model, action_range=action_range)

    agent.model.share_memory()

    trainer = PPOCrowdTrainer(agent, args.env, config)
    trainer.train(args.iters, disable_tqdm=False, save_path=trainer.path)
