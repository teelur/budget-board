import classes from "../Navbar.module.css";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { Tooltip, UnstyledButton, Group } from "@mantine/core";

interface NavbarLinkProps {
  icon: React.ReactNode;
  label: string;
  active?: boolean;
  onClick?: () => void;
  showLabel?: boolean;
  className?: string;
  labelSize?: "sm" | "xs";
  compact?: boolean;
}

const NavbarLink = (props: NavbarLinkProps): React.ReactNode => {
  return (
    <Tooltip
      label={props.label}
      position="right"
      disabled={props.showLabel}
      transitionProps={{ duration: 0 }}
    >
      <UnstyledButton
        onClick={props.onClick}
        aria-label={!props.showLabel ? props.label : undefined}
        className={`${classes.link} ${!props.showLabel ? classes.collapsed : ""} ${props.compact ? classes.compact : ""} ${props.className ?? ""}`}
        data-active={props.active || undefined}
      >
        <Group
          justify={props.showLabel ? "flex-start" : "center"}
          gap="xs"
          wrap="nowrap"
          w="100%"
        >
          {props.icon}
          {props.showLabel && (
            <PrimaryText
              size={props.labelSize ?? "sm"}
              c="var(--navbar-link-color)"
            >
              {props.label}
            </PrimaryText>
          )}
        </Group>
      </UnstyledButton>
    </Tooltip>
  );
};

export default NavbarLink;
