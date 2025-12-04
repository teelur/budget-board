import ProgressBase, { ProgressBaseProps } from "../ProgressBase/ProgressBase";

export interface ElevatedProgressProps extends ProgressBaseProps {}

const ElevatedProgress = ({ bg, ...props }: ElevatedProgressProps) => {
  return (
    <ProgressBase {...props} bg={bg ?? "var(--elevated-color-progress)"} />
  );
};

export default ElevatedProgress;
