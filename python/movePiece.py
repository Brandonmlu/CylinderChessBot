import argparse
from dataclasses import dataclass
from peaceful_pie.unity_comms import UnityComms

@dataclass
class MyVector3:
    x: float
    y: float
    z: float

def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    position = MyVector3(args.x, args.y, 10.0)
    unity_comms.MovePiece(position=position)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    parser.add_argument('--x', type=float, default=0)
    parser.add_argument('--y', type=float, default=0)
    args = parser.parse_args()
    run(args)