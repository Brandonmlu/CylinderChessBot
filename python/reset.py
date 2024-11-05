import argparse
from dataclasses import dataclass
from peaceful_pie.unity_comms import UnityComms
from typing import Any, Tuple
import numpy as np
from numpy.typing import NDArray

@dataclass
class MyVector3:
    x: float
    y: float
    z: float

@dataclass
class Observation:
    board_state: NDArray[np.int8]
    selected_piece: int
    in_check: int

def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    res = unity_comms.Reset(ResultClass=Observation)
    print(res)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)