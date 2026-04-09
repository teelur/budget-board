import classes from "./SettingsNavLink.module.css";
import { UnstyledButton } from "@mantine/core";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";

interface SettingsNavLinkProps {
  label: string;
  active?: boolean;
  onClick?: () => void;
}

const SettingsNavLink = (props: SettingsNavLinkProps): React.ReactNode => {
  return (
    <UnstyledButton
      className={classes.link}
      data-active={props.active || undefined}
      onClick={props.onClick}
    >
      <PrimaryText
        size="sm"
        c={props.active ? "var(--mantine-primary-color-contrast)" : undefined}
      >
        {props.label}
      </PrimaryText>
    </UnstyledButton>
  );
};

export default SettingsNavLink;
