# -*- coding: utf-8 -*-
import click
from pathlib import Path
from pydantic import BaseModel, Field
# import yaml

class Mod(BaseModel):
    """模组信息"""
    title: str = Field(..., description="模组标题")
    staticID: str = Field(..., description="模组静态ID")
    description: str = Field(..., description="模组描述")

class ModInfo(BaseModel):
    """模组信息"""
    supportedContent: str = Field("all", description="支持的内容")
    minimumSupportedBuild: str = Field(..., description="最低支持的版本")
    version: str = Field(..., description="模组版本")
    APIVersion : str = Field(..., description="API版本")

@click.group()
def cli():
    """ONI-Mods 命令行工具集 - 用于创建和管理ONI模组项目"""
    pass

