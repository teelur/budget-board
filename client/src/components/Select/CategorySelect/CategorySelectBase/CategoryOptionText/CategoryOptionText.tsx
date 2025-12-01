import { TextProps as MantineTextProps } from "@mantine/core";
import DimmedText from "~/components/Text/DimmedText/DimmedText";
import PrimaryText from "~/components/Text/PrimaryText/PrimaryText";

interface CategoryOptionTextProps extends MantineTextProps {
  isParent?: boolean;
  children: React.ReactNode;
}

const CategoryOptionText = ({
  isParent = false,
  children,
  ...props
}: CategoryOptionTextProps): React.ReactNode => {
  return isParent ? (
    <PrimaryText {...props}>{children}</PrimaryText>
  ) : (
    <DimmedText {...props}>{children}</DimmedText>
  );
};

export default CategoryOptionText;
