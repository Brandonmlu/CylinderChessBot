import argparse
from peaceful_pie.unity_comms import UnityComms

def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    test = unity_comms.isValidPieceMove(index=args.index, piece=args.piece)
    print("VALID PIECE MOVE?", test)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--index', type=int, required=True)
    parser.add_argument('--piece', type=int, required=True)
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)