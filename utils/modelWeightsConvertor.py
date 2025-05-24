import argparse
from ultralytics import YOLO

def main():
    parser = argparse.ArgumentParser(description='Экспорт модели YOLO в формат ONNX')
    parser.add_argument('--model-path', type=str, required=True, help='Путь до YOLO модели (.pt)')
    parser.add_argument('--output-path', type=str, required=True, help='Путь для экспорта ONNX модели')
    args = parser.parse_args()

    model = YOLO(args.model_path)
    model.export(format='onnx', save_dir=args.output_path)

if __name__ == "__main__":
    main()