import { Group } from "@mantine/core";
import { ActionIcon } from "@teelur/budget-board-ui";
import React from "react";
import { ArrowLeftIcon, ChevronRightIcon } from "lucide-react";
import { useNavigate } from "react-router";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import SecondaryHeading from "~/components/core/Heading/SecondaryHeading/SecondaryHeading";

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
      <PrimaryHeading order={5}>{title}</PrimaryHeading>
      {activeItem && (
        <>
          <ChevronRightIcon size="1rem" color="var(--base-color-text-dimmed)" />
          <SecondaryHeading order={5}>{activeItem}</SecondaryHeading>
        </>
      )}
    </Group>
  );
};

export default SettingsHeading;
