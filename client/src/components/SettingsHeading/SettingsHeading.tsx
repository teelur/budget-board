import { Group } from "@mantine/core";
import { ActionIcon, HeadingText } from "@teelur/budget-board-ui";
import React from "react";
import { ArrowLeftIcon, ChevronRightIcon } from "lucide-react";
import { useNavigate } from "react-router";

interface SettingsHeadingProps {
  title: React.ReactNode;
  backTo: string;
  activeItem?: React.ReactNode;
}

const SettingsHeading = ({
  title,
  backTo,
  activeItem,
}: SettingsHeadingProps): React.ReactNode => {
  const navigate = useNavigate();

  return (
    <Group gap="xs">
      <ActionIcon
        variant="ghost"
        color="primary"
        size="compact-sm"
        onClick={() => navigate(backTo)}
      >
        <ArrowLeftIcon />
      </ActionIcon>
      <HeadingText tone="heading" level={5}>
        {title}
      </HeadingText>
      {activeItem && (
        <>
          <ChevronRightIcon
            size="1rem"
            color="var(--bbui-color-text-primary)"
          />
          <HeadingText tone="secondary" level={5}>
            {activeItem}
          </HeadingText>
        </>
      )}
    </Group>
  );
};

export default SettingsHeading;
