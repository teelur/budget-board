import { ActionIcon, Group } from "@mantine/core";
import React from "react";
import { SiGithub } from "@icons-pack/react-simple-icons";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

const Footer = (): React.ReactNode => {
  return (
    <Group gap="0.5rem">
      <PrimaryText size="xs">{import.meta.env.VITE_VERSION}</PrimaryText>
      <ActionIcon
        component="a"
        href="https://github.com/teelur/budget-board"
        target="_blank"
        variant="subtle"
        color="var(--base-color-text-primary)"
      >
        <SiGithub />
      </ActionIcon>
    </Group>
  );
};

export default Footer;
