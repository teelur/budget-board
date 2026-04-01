import classes from "../Navbar.module.css";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { Tooltip, UnstyledButton, Group } from "@mantine/core";

interface NavbarLinkProps {
  icon: React.ReactNode;
  label: string;
  active?: boolean;
  onClick?: () => void;
}

const NavbarLink = (props: NavbarLinkProps): React.ReactNode => {
  return (
    <Tooltip
      label={props.label}
      position="right"
      transitionProps={{ duration: 0 }}
    >
      <UnstyledButton
        onClick={props.onClick}
        p="0.75rem 0.5rem"
        className={classes.link}
        data-active={props.active || undefined}
      >
        <Group>
          {props.icon}
          <PrimaryText hiddenFrom="xs">{props.label}</PrimaryText>
        </Group>
      </UnstyledButton>
    </Tooltip>
  );
};

export default NavbarLink;
