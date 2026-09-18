import { ActionIcon, Group, Stack, Text, Tooltip } from "@mantine/core";
import { ArrowUpRightIcon, CircleArrowUpIcon } from "lucide-react";
import { SiGithub } from "@icons-pack/react-simple-icons";
import React from "react";
import { useTranslation } from "react-i18next";
import classes from "../Navbar.module.css";
import { APP_REPOSITORY } from "~/helpers/appUpdates";
import { useAppUpdateQuery } from "~/hooks/queries/useAppUpdateQuery";

interface NavbarFooterProps {
  showExpandedNav: boolean;
}

const NavbarFooter = ({
  showExpandedNav,
}: NavbarFooterProps): React.ReactNode => {
  const { t } = useTranslation();
  const { data: update } = useAppUpdateQuery();
  const version = import.meta.env.VITE_VERSION;
  const repositoryUrl = `https://github.com/${APP_REPOSITORY}`;
  const updateLabel = update
    ? t(
        update.channel === "stable"
          ? "update_available"
          : "dev_update_available",
        { version: update.latestVersion },
      )
    : undefined;
  const currentVersionLabel = t("current_version", { version });
  const versionTextRef = React.useRef<HTMLParagraphElement>(null);
  const [isVersionTruncated, setIsVersionTruncated] = React.useState(false);

  React.useEffect(() => {
    const versionText = versionTextRef.current;
    if (!versionText) {
      return;
    }

    const updateTruncationState = () => {
      setIsVersionTruncated(versionText.scrollWidth > versionText.clientWidth);
    };

    updateTruncationState();
    const observer = new ResizeObserver(updateTruncationState);
    observer.observe(versionText);

    return () => observer.disconnect();
  }, [currentVersionLabel]);

  if (!showExpandedNav) {
    return (
      <Stack
        className={`${classes.footer} ${classes.footerCollapsed}`}
        gap="xs"
      >
        {update && updateLabel && (
          <Tooltip label={updateLabel} position="right">
            <ActionIcon
              component="a"
              href={update.url}
              target="_blank"
              rel="noreferrer"
              aria-label={updateLabel}
              className={classes.updateIcon}
              variant="subtle"
            >
              <CircleArrowUpIcon size="1.25rem" />
            </ActionIcon>
          </Tooltip>
        )}
        <Tooltip label={currentVersionLabel} position="right">
          <Text className={classes.collapsedVersion} size="xs" ta="center">
            {version}
          </Text>
        </Tooltip>
      </Stack>
    );
  }

  return (
    <Stack className={classes.footer} gap="xs">
      {update && updateLabel && (
        <a
          href={update.url}
          target="_blank"
          rel="noreferrer"
          aria-label={updateLabel}
          className={classes.updateLink}
        >
          <Group gap="xs" wrap="nowrap">
            <CircleArrowUpIcon className={classes.updateGlyph} size="1.25rem" />
            <Stack className={classes.updateText} gap={0}>
              <Text className={classes.updateTitle} size="xs" fw={700}>
                {updateLabel}
              </Text>
              <Text size="xs" c="var(--base-color-text-secondary)" truncate>
                {t("view_update")}
              </Text>
            </Stack>
            <ArrowUpRightIcon size="0.9rem" />
          </Group>
        </a>
      )}
      <Group
        className={classes.versionRow}
        gap="xs"
        justify="space-between"
        wrap="nowrap"
      >
        <Tooltip
          label={currentVersionLabel}
          position="right"
          disabled={!isVersionTruncated}
        >
          <Text
            ref={versionTextRef}
            className={classes.versionText}
            size="xs"
            c="var(--base-color-text-primary)"
            fw={500}
            truncate
          >
            {currentVersionLabel}
          </Text>
        </Tooltip>
        <Tooltip label={t("github_repository")} position="right">
          <ActionIcon
            component="a"
            href={repositoryUrl}
            target="_blank"
            rel="noreferrer"
            aria-label={t("github_repository")}
            className={classes.footerIcon}
            variant="subtle"
          >
            <SiGithub />
          </ActionIcon>
        </Tooltip>
      </Group>
    </Stack>
  );
};

export default NavbarFooter;
