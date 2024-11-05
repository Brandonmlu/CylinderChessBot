import argparse
from dataclasses import dataclass
from peaceful_pie.unity_comms import UnityComms

def run(args: argparse.Namespace) -> None:
    unity_comms = UnityComms(port=args.port)
    matrix = unity_comms.GetBoardMatrix()
    
    '''
    for y in range(len(matrix)-1, -1, -1):
        row = ""
        for x in range(len(matrix)):
            row += str(matrix[x][y]) + " "
        print(row)
    '''

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=9000)
    args = parser.parse_args()
    run(args)