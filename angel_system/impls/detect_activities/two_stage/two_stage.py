"""

TODO:
* Update documentation

"""

import pdb

import torch
from torch import nn
# from torchmetrics import MaxMetric
# from torchmetrics.classification.accuracy import Accuracy

from .spatial.fcn import SpatialFCNModule
from .temporal.rulstm import RULSTM


class TwoStageModule(nn.Module):
    """This class implements the spatio-temporal model used for unified
    representation of multi-modal inputs in the scene. This model also
    performs the activity recognition for the given frame sequence.

    Args: TBD
    """

    def __init__(self, checkpoint: str, num_classes: int):
        super().__init__()

        self.fcn = SpatialFCNModule('resnext')
        self.temporal = RULSTM(num_classes, hidden=128, dropout=0, depth=3)

    def forward(self, data):
        frame_feats = self.fcn(data)
        # pdb.set_trace()
        out = self.temporal(frame_feats.unsqueeze(1))

        return out

