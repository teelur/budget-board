import { notifications } from "@mantine/notifications";
import { AxiosError } from "axios";
import React from "react";
import { useQueryClient } from "@tanstack/react-query";
import { useTranslation } from "react-i18next";
import {
  accountsQueryKey,
  balancesQueryKey,
  institutionsQueryKey,
  transactionsQueryKey,
} from "~/helpers/requests";
import { ITransactionImportJobResponse } from "~/models/transaction";
import {
  transactionImportJobTerminalStatuses,
  useTransactionImportJobQuery,
} from "~/hooks/queries/useTransactionImportJobQuery";

const transactionImportJobStorageKey = "budget-board:transaction-import-job";

interface TransactionImportJobContextValue {
  activeJobId: string | null;
  job: ITransactionImportJobResponse | undefined;
  isLoading: boolean;
  startImport: (jobId: string) => void;
}

export const TransactionImportJobContext =
  React.createContext<TransactionImportJobContextValue | null>(null);

const getStoredJobId = () => {
  if (typeof window === "undefined") {
    return null;
  }

  return window.localStorage.getItem(transactionImportJobStorageKey);
};

const removeStoredJobId = () => {
  if (typeof window !== "undefined") {
    window.localStorage.removeItem(transactionImportJobStorageKey);
  }
};

export const TransactionImportJobProvider = ({
  children,
}: {
  children: React.ReactNode;
}): React.ReactNode => {
  const { t } = useTranslation();
  const queryClient = useQueryClient();
  const [activeJobId, setActiveJobId] = React.useState<string | null>(
    getStoredJobId,
  );
  const [completedJob, setCompletedJob] = React.useState<
    ITransactionImportJobResponse | undefined
  >();
  const handledJobId = React.useRef<string | null>(null);
  const activeJobIdRef = React.useRef(activeJobId);
  const importJobQuery = useTransactionImportJobQuery(activeJobId);

  activeJobIdRef.current = activeJobId;

  React.useEffect(() => {
    const error = importJobQuery.error as AxiosError | null;
    if (!importJobQuery.isError || error?.response?.status !== 404) {
      return;
    }

    removeStoredJobId();
    setActiveJobId(null);
  }, [importJobQuery.error, importJobQuery.isError]);

  React.useEffect(() => {
    const importJob = importJobQuery.data;
    if (
      !importJob ||
      handledJobId.current === importJob.id ||
      !transactionImportJobTerminalStatuses.includes(
        importJob.status as (typeof transactionImportJobTerminalStatuses)[number],
      )
    ) {
      return;
    }

    handledJobId.current = importJob.id;

    if (importJob.status === "Failed") {
      setCompletedJob(importJob);
      removeStoredJobId();
      setActiveJobId(null);
      notifications.show({
        color: "var(--button-color-destructive)",
        message: t("import_failed"),
      });
      return;
    }

    void Promise.all([
      queryClient.invalidateQueries({ queryKey: [transactionsQueryKey] }),
      queryClient.invalidateQueries({ queryKey: [balancesQueryKey] }),
      queryClient.invalidateQueries({ queryKey: [accountsQueryKey] }),
      queryClient.invalidateQueries({ queryKey: [institutionsQueryKey] }),
    ]).then(() => {
      if (activeJobIdRef.current !== importJob.id) {
        return;
      }

      setCompletedJob(importJob);
      removeStoredJobId();
      setActiveJobId(null);
      notifications.show({
        color: "var(--button-color-success)",
        message: t(
          importJob.status === "CompletedWithErrors"
            ? "import_completed_with_errors"
            : "import_completed_successfully",
        ),
      });
    });
  }, [importJobQuery.data, queryClient, t]);

  const startImport = (jobId: string) => {
    window.localStorage.setItem(transactionImportJobStorageKey, jobId);
    handledJobId.current = null;
    setCompletedJob(undefined);
    setActiveJobId(jobId);
  };

  const value = {
    activeJobId,
    job: importJobQuery.data ?? completedJob,
    isLoading: activeJobId !== null && importJobQuery.isPending,
    startImport,
  };

  return (
    <TransactionImportJobContext.Provider value={value}>
      {children}
    </TransactionImportJobContext.Provider>
  );
};

export const useTransactionImportJob = () => {
  const context = React.useContext(TransactionImportJobContext);
  if (!context) {
    throw new Error(
      "useTransactionImportJob must be used within TransactionImportJobProvider",
    );
  }

  return context;
};
