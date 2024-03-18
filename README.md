# Crowd AI

This repository contains the crowd simulation setup that my PhD research is largely based on.

Publications that use this project:
- Understanding Reinforcement Learned Crowds (v6) (MIG 2022/Computers & Graphics)

A more detailed documentation will be added soon.

## Understanding Reinforcement Learned Crowds
The experiments were run with the CrowdAI-v6 release, corresponding to the v6a tag. 

## v6a tested and works on Mac OSX M1 Pro following these steps:
- conda install grpcio
- conda create -n mlagents python=3.10.12 && conda activate mlagents
- pip install -r requirements.txt
  - note that there are incompatability issues betweeen onnx and protobuf versions, these issues should be resolved with the above reqs file
  - https://www.ame-name.com/archives/16760 
- create a unity project based on this repo, start the project in unity, and in the terminal cd into that folder, create a new folder for training models, and cd into that folder
- in the terminal run: mlagents-learn
- In unity, click 'play'
- Based on:
  -  https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Installation.md
  -  https://github.com/Unity-Technologies/ml-agents/blob/develop/docs/Getting-Started.md


## Existing issues:
- warning message: **Unknown side channel data received. Channel type: 621f0a70-4f87-11ea-a6bf-784f4387d1f7**
- see: https://github.com/huggingface/ml-agents-patch/blob/develop/docs/Custom-SideChannels.md 
