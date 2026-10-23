import { Group, Stack } from "@mantine/core";
import { BodyText, Skeleton } from "@teelur/budget-board-ui";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { IGoalResponse } from "~/models/goal";
import { AxiosError } from "axios";
import React from "react";
import GoalCard from "./GoalCard/GoalCard";
import GoalsHeader from "./GoalsHeader/GoalsHeader";
import { NotificationType, showNotification } from "~/helpers/notifications";
import { translateAxiosError } from "~/helpers/requests";
import GoalDetails from "./GoalDetails/GoalDetails";
import { useTranslation } from "react-i18next";
import { InfoIcon } from "lucide-react";
import { useGoalsQuery } from "~/hooks/queries/useGoalsQuery";
import CompletedGoalCard from "./CompletedGoalCard/CompletedGoalCard";

export enum GoalCategory {
  InProgress = "in_progress",
  Completed = "completed",
}

const Goals = (): React.ReactNode => {
  const [includeInterest, { toggle: toggleIncludeInterest }] = useDisclosure();
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure();
  const [goalCategory, setGoalCategory] = React.useState<string>(
    GoalCategory.InProgress,
  );

  const { t } = useTranslation();
  const goalsQuery = useGoalsQuery({ includeInterest });

  const [selectedGoal, setSelectedGoal] = React.useState<IGoalResponse | null>(
    null,
  );

  React.useEffect(() => {
    if (goalsQuery.isError) {
      showNotification({
        type: NotificationType.Error,
        message: translateAxiosError(goalsQuery.error as AxiosError),
      });
    }
  }, [goalsQuery.isError]);

  useDidUpdate(() => {
    goalsQuery.refetch();
  }, [includeInterest]);

  const activeGoals = React.useMemo(
    () => (goalsQuery.data ?? []).filter((goal) => goal.completed == null),
    [goalsQuery.data],
  );

  const completedGoals = React.useMemo(
    () => (goalsQuery.data ?? []).filter((goal) => goal.completed != null),
    [goalsQuery.data],
  );

  const openGoalDetails = (goal: IGoalResponse) => {
    setSelectedGoal(goal);
    openDetails();
  };

  const getContent = () => {
    if (goalsQuery.isPending) {
      return <Skeleton h={100} w="100%" />;
    }

    if (goalCategory === GoalCategory.Completed) {
      return completedGoals.length === 0 ? (
        <Group justify="center" align="center" gap="0.5rem">
          <InfoIcon size={20} color="var(--bbui-color-text-muted)" />
          <BodyText tone="muted" size="sm">
            {t("no_goals")}
          </BodyText>
        </Group>
      ) : (
        <Stack gap="0.5rem">
          {completedGoals.map((goal: IGoalResponse) => (
            <CompletedGoalCard key={goal.id} goal={goal} />
          ))}
        </Stack>
      );
    }
    if (goalCategory === GoalCategory.InProgress) {
      return activeGoals.length === 0 ? (
        <Group justify="center" align="center" gap="0.5rem">
          <InfoIcon size={20} color="var(--bbui-color-text-muted)" />
          <BodyText tone="muted" size="sm">
            {t("no_goals")}
          </BodyText>
        </Group>
      ) : (
        <Stack gap="0.5rem">
          {activeGoals.map((goal: IGoalResponse) => (
            <GoalCard
              key={goal.id}
              goal={goal}
              includeInterest={includeInterest}
              openGoalDetails={openGoalDetails}
            />
          ))}
        </Stack>
      );
    }
    return null;
  };

  return (
    <Stack w="100%" maw={1400} gap="0.75rem">
      <GoalDetails
        goal={selectedGoal}
        isOpen={isDetailsOpen}
        doClose={closeDetails}
      />
      <GoalsHeader
        includeInterest={includeInterest}
        toggleIncludeInterest={toggleIncludeInterest}
        goalCategory={goalCategory}
        setGoalCategory={setGoalCategory}
      />
      {getContent()}
    </Stack>
  );
};

export default Goals;
