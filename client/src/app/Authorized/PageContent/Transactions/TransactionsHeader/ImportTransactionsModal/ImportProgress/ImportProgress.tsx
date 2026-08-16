import { Progress, Stack } from "@mantine/core";
import PrimaryText from "~/components/core/Text/PrimaryText/PrimaryText";
import { ITransactionImportJobResponse } from "~/models/transaction";
import { useTranslation } from "react-i18next";

interface ImportProgressProps {
  job: ITransactionImportJobResponse | undefined;
  isLoading: boolean;
}

const ImportProgress = (props: ImportProgressProps) => {
  const { t } = useTranslation();
  const isFailed = props.job?.status === "Failed";

  return (
    <Stack justify="center" gap="0.75rem" w={600} maw="100%" py="1rem">
      <PrimaryText size="md">
        {isFailed
          ? t("import_failed")
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
            color={isFailed ? "red" : undefined}
            animated={!isFailed}
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
        </>
      )}
    </Stack>
  );
};

export default ImportProgress;
