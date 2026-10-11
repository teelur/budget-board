import { Group, Stack } from "@mantine/core";
import { BodyText, Button, Progress } from "@teelur/budget-board-ui";
import { BanIcon } from "lucide-react";
import React from "react";
import { ITransactionImportJobResponse } from "~/models/transaction";
import { useTranslation } from "react-i18next";

interface ImportProgressProps {
  job: ITransactionImportJobResponse | undefined;
  isLoading: boolean;
  isCancelling?: boolean;
  onCancel?: () => Promise<void>;
}

const ImportProgress = (props: ImportProgressProps) => {
  const { t } = useTranslation();
  const [isConfirmingCancel, setIsConfirmingCancel] = React.useState(false);
  const isFailed = props.job?.status === "Failed";
  const isCancelled = props.job?.status === "Cancelled";
  const isCancellationRequested = props.job?.cancellationRequested ?? false;
  const canCancel =
    props.onCancel &&
    props.job &&
    !isFailed &&
    !isCancelled &&
    !isCancellationRequested;
  const statusMessage = isFailed
    ? t("import_failed")
    : isCancelled
      ? t("import_stopped")
      : isCancellationRequested
        ? t("import_stop_requested")
        : props.job?.status === "Pending"
          ? t("import_queued")
          : t("import_in_progress");
  const animated = !isFailed && !isCancelled && !isCancellationRequested;

  const confirmCancel = async () => {
    setIsConfirmingCancel(false);
    await props.onCancel?.();
  };

  return (
    <Stack
      justify="center"
      gap="0.75rem"
      w={600}
      maw="100%"
      mx="auto"
      py="1rem"
    >
      <BodyText tone="primary" size="md" fw={500}>
        {statusMessage}
      </BodyText>
      {props.isLoading && !props.job ? (
        <Progress value={0} animated striped ariaLabel={statusMessage} />
      ) : (
        <>
          <Progress
            value={props.job?.progressPercentage ?? 0}
            color={isFailed ? "error" : isCancelled ? "warning" : undefined}
            animated={animated}
            striped={animated}
            ariaLabel={statusMessage}
          />
          <BodyText tone="secondary" size="sm">
            {t("import_progress", {
              processed: props.job?.processedCount ?? 0,
              total: props.job?.totalCount ?? 0,
            })}
          </BodyText>
          {props.job?.errorMessage ? (
            <BodyText tone="secondary" size="sm">
              {props.job.errorMessage}
            </BodyText>
          ) : null}
          {canCancel && !isConfirmingCancel ? (
            <Button
              variant="filled"
              color="error"
              size="compact-sm"
              leftSection={<BanIcon size={16} />}
              loading={props.isCancelling}
              onClick={() => setIsConfirmingCancel(true)}
            >
              {t("stop")}
            </Button>
          ) : null}
          {canCancel && isConfirmingCancel ? (
            <Stack gap="xs">
              <BodyText tone="secondary" size="sm">
                {t("confirm_stop_import_message")}
              </BodyText>
              <Group grow>
                <Button
                  variant="filled"
                  color="neutral"
                  size="compact-sm"
                  onClick={() => setIsConfirmingCancel(false)}
                >
                  {t("cancel")}
                </Button>
                <Button
                  variant="filled"
                  color="error"
                  size="compact-sm"
                  onClick={() => void confirmCancel()}
                  loading={props.isCancelling}
                >
                  {t("confirm_stop")}
                </Button>
              </Group>
            </Stack>
          ) : null}
        </>
      )}
    </Stack>
  );
};

export default ImportProgress;
