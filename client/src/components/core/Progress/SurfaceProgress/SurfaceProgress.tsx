import ProgressBase, { ProgressBaseProps } from "../ProgressBase/ProgressBase";

export interface SurfaceProgressProps extends ProgressBaseProps {}

const SurfaceProgress = ({ ...props }: SurfaceProgressProps) => {
  return (
    <ProgressBase {...props} bg={props.bg ?? "var(--surface-color-progress)"} />
  );
};

export default SurfaceProgress;
