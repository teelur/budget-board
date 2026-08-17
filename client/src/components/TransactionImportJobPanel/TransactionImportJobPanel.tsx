import {
  Affix,
  Button,
  FloatingWindow,
  Group,
  Progress,
  Stack,
  Transition,
} from "@mantine/core";
import type { SetFloatingWindowPosition } from "@mantine/hooks";
import {
  BanIcon,
  ChevronDownIcon,
  ChevronUpIcon,
  CircleCheckIcon,
  LoaderCircleIcon,
  CircleXIcon,
  XIcon,
} from "lucide-react";
import React from "react";
import { useTranslation } from "react-i18next";
import PrimaryHeading from "~/components/core/Heading/PrimaryHeading/PrimaryHeading";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { transactionImportJobTerminalStatuses } from "~/hooks/queries/useTransactionImportJobQuery";
import { useTransactionImportJob } from "~/providers/TransactionImportJobProvider/TransactionImportJobProvider";
import classes from "./TransactionImportJobPanel.module.css";

const panelCollapsedStorageKey =
  "budget-board:transaction-import-panel-collapsed";

const getStoredPanelCollapsed = () =>
  typeof window !== "undefined" &&
  window.localStorage.getItem(panelCollapsedStorageKey) === "true";

const getInitialPosition = () => {
  if (typeof window === "undefined") {
    return { bottom: 10, left: 16 };
  }

  return {
    bottom: 10,
    left: Math.max(16, (window.innerWidth - 384) / 2),
  };
};

const TransactionImportJobPanel = () => {
  const { t } = useTranslation();
  const {
    activeJobId,
    job,
    isLoading,
    isCancelling,
    cancelImport,
    dismissImport,
  } = useTransactionImportJob();
  const [isConfirmingCancel, setIsConfirmingCancel] = React.useState(false);
  const [isCollapsed, setIsCollapsed] = React.useState(getStoredPanelCollapsed);
  const setPositionRef = React.useRef<SetFloatingWindowPosition | null>(null);

  React.useLayoutEffect(() => {
    setPositionRef.current?.({ bottom: 16 });
  }, [isConfirmingCancel]);

  if (!activeJobId && !job) {
    return null;
  }

  const status = job?.status;
  const isTerminal = status
    ? transactionImportJobTerminalStatuses.includes(
        status as (typeof transactionImportJobTerminalStatuses)[number],
      )
    : false;
  const isCancellationRequested = job?.cancellationRequested ?? false;
  const isActive = Boolean(activeJobId && !isTerminal);
  const statusMessage =
    status === "Failed"
      ? t("import_failed")
      : status === "Cancelled"
        ? t("import_cancelled")
        : status === "CompletedWithErrors"
          ? t("import_completed_with_errors")
          : status === "Completed"
            ? t("import_completed_successfully")
            : isCancellationRequested
              ? t("import_cancellation_requested")
              : status === "Pending"
                ? t("import_queued")
                : t("import_in_progress");
  const progressColor =
    status === "Failed"
      ? "red"
      : status === "Cancelled" || isCancellationRequested
        ? "yellow"
        : undefined;

  const handleCancel = () => {
    setIsConfirmingCancel(true);
  };

  const confirmCancel = async () => {
    setIsConfirmingCancel(false);
    await cancelImport();
  };

  const toggleCollapsed = () => {
    setIsCollapsed((collapsed) => {
      const nextCollapsed = !collapsed;
      window.localStorage.setItem(
        panelCollapsedStorageKey,
        String(nextCollapsed),
      );
      return nextCollapsed;
    });
  };

  const collapsedStatus =
    status === "Cancelled"
      ? t("import_cancelled")
      : status === "Failed"
        ? t("import_failed")
        : isActive
          ? t("import_in_progress")
          : t("import_complete");
  const collapsedStatusIcon =
    status === "Cancelled" || status === "Failed" ? (
      <CircleXIcon className={classes.statusIndicator} size={16} />
    ) : isActive ? (
      <LoaderCircleIcon
        className={`${classes.statusIndicator} ${classes.loadingIndicator}`}
        size={16}
      />
    ) : (
      <CircleCheckIcon className={classes.statusIndicator} size={16} />
    );

  return (
    <>
      <Transition mounted={!isCollapsed} transition="slide-up" duration={220}>
        {(styles) => (
          <FloatingWindow
            className={classes.root}
            w="min(24rem, calc(100vw - 2rem))"
            shadow="md"
            p="md"
            radius="sm"
            withBorder
            initialPosition={getInitialPosition()}
            constrainToViewport
            constrainOffset={16}
            setPositionRef={setPositionRef}
            dragHandleSelector={`.${classes.dragHandle}`}
            excludeDragHandleSelector="button"
            style={{ ...styles, cursor: "move" }}
          >
            <Stack gap="sm">
              <Group
                className={classes.dragHandle}
                justify="space-between"
                align="flex-start"
                wrap="nowrap"
              >
                <Stack gap={0}>
                  <PrimaryHeading order={5}>
                    {t("import_transactions")}
                  </PrimaryHeading>
                  <PrimaryText size="sm">{statusMessage}</PrimaryText>
                </Stack>
                <Group gap={4} wrap="nowrap">
                  {isTerminal && (
                    <Button
                      variant="subtle"
                      size="compact-sm"
                      p={4}
                      aria-label={t("dismiss")}
                      onClick={dismissImport}
                    >
                      <XIcon size={16} />
                    </Button>
                  )}
                  <Button
                    className={classes.collapseButton}
                    variant="subtle"
                    size="compact-sm"
                    p={4}
                    aria-label={t("hide_import_panel")}
                    onClick={toggleCollapsed}
                  >
                    <ChevronDownIcon size={16} />
                  </Button>
                </Group>
              </Group>
              {isLoading && !job ? (
                <Progress value={0} animated />
              ) : (
                <>
                  <Progress
                    value={job?.progressPercentage ?? 0}
                    color={progressColor}
                    animated={!isTerminal && !isCancellationRequested}
                  />
                  <PrimaryText size="sm">
                    {t("import_progress", {
                      processed: job?.processedCount ?? 0,
                      total: job?.totalCount ?? 0,
                    })}
                  </PrimaryText>
                  {job?.errorMessage ? (
                    <PrimaryText size="sm" c="red">
                      {job.errorMessage}
                    </PrimaryText>
                  ) : null}
                </>
              )}
              {activeJobId && !isTerminal && !isConfirmingCancel && (
                <Button
                  color="red"
                  variant="outline"
                  leftSection={<BanIcon size={16} />}
                  loading={isCancelling}
                  disabled={isCancellationRequested}
                  onClick={handleCancel}
                >
                  {isCancellationRequested
                    ? t("import_cancelling")
                    : t("cancel")}
                </Button>
              )}
              {activeJobId && !isTerminal && isConfirmingCancel && (
                <Stack gap="xs">
                  <PrimaryText size="sm">
                    {t("confirm_cancel_import_message")}
                  </PrimaryText>
                  <Group grow>
                    <Button
                      variant="default"
                      onClick={() => setIsConfirmingCancel(false)}
                    >
                      {t("cancel")}
                    </Button>
                    <Button
                      color="red"
                      onClick={() => void confirmCancel()}
                      loading={isCancelling}
                    >
                      {t("confirm_cancel_import")}
                    </Button>
                  </Group>
                </Stack>
              )}
            </Stack>
          </FloatingWindow>
        )}
      </Transition>
      <Affix
        position={{ bottom: 0, left: "50%" }}
        style={{ transform: "translateX(-50%)" }}
      >
        <Transition mounted={isCollapsed} transition="slide-up" duration={220}>
          {(styles) => (
            <Button
              className={classes.tab}
              size="sm"
              aria-label={t("show_import_panel")}
              onClick={toggleCollapsed}
              leftSection={collapsedStatusIcon}
              rightSection={<ChevronUpIcon size={16} />}
              style={styles}
            >
              {collapsedStatus}
            </Button>
          )}
        </Transition>
      </Affix>
    </>
  );
};

export default TransactionImportJobPanel;
