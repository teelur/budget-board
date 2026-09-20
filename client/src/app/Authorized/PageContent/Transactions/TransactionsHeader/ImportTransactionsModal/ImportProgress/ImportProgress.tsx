import { Group, Progress, Stack } from "@mantine/core";
import { Button } from "@teelur/budget-board-ui";
import { BanIcon } from "lucide-react";
import React from "react";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
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
      <PrimaryText size="md">
        {isFailed
          ? t("import_failed")
          : isCancelled
            ? t("import_stopped")
            : isCancellationRequested
              ? t("import_stop_requested")
              : props.job?.status === "Pending"
                ? t("import_queued")
                : t("import_in_progress")}
      </PrimaryText>
      {props.isLoading && !props.job ? (
        <Progress value={0} animated />
      ) : (
        <>
          <Progress
            value={props.job?.progressPercentage ?? 0}
            color={isFailed ? "red" : isCancelled ? "yellow" : undefined}
            animated={!isFailed && !isCancelled && !isCancellationRequested}
          />
          <PrimaryText size="sm">
            {t("import_progress", {
              processed: props.job?.processedCount ?? 0,
              total: props.job?.totalCount ?? 0,
            })}
          </PrimaryText>
          {props.job?.errorMessage ? (
            <PrimaryText size="sm" c="red">
              {props.job.errorMessage}
            </PrimaryText>
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
              <PrimaryText size="sm">
                {t("confirm_stop_import_message")}
              </PrimaryText>
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
