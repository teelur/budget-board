import React from "react";
import Card, { CardProps } from "~/components/core/Card/Card";
import { Flex, Stack } from "@mantine/core";
import Divider from "~/components/core/Divider/Divider";

export enum BorderThickness {
  Normal,
  Thick,
}

interface SplitCardProps extends CardProps {
  border?: BorderThickness;
  header: React.ReactNode;
  elevation?: number;
  children: React.ReactNode;
}

const SplitCard = ({
  border,
  header,
  elevation,
  style,
  children,
  ...props
}: SplitCardProps): React.ReactNode => {
  return (
    <Card
      w="100%"
      p={0}
      radius="sm"
      {...props}
      style={{
        ...style,
        borderWidth: border === BorderThickness.Thick ? "2px" : "1px",
        display: "flex",
        flexDirection: "column",
      }}
      elevation={elevation}
    >
      <Stack gap={0} style={{ flex: 1, minHeight: 0 }}>
        <Flex w="100%" p="0.5rem" direction="column" align="stretch">
          {header}
        </Flex>
        <Divider
          size={border === BorderThickness.Thick ? "sm" : "xs"}
          elevation={elevation}
        />
        <Flex
          w="100%"
          p="0.5rem"
          direction="column"
          align="stretch"
          style={{ flex: 1, minHeight: 0, overflowY: "auto" }}
        >
          {children}
        </Flex>
      </Stack>
    </Card>
  );
};

export default SplitCard;
