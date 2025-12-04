import ProgressBase, { ProgressBaseProps } from "../ProgressBase/ProgressBase";

export interface BaseProgressProps extends ProgressBaseProps {}

const BaseProgress = ({ ...props }: BaseProgressProps) => {
  return (
    <ProgressBase {...props} bg={props.bg ?? "var(--base-color-progress)"} />
  );
};

export default BaseProgress;
