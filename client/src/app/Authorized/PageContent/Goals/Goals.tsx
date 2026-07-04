import { Group, Skeleton, Stack } from "@mantine/core";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { IGoalResponse } from "~/models/goal";
import { AxiosError } from "axios";
import React from "react";
import GoalCard from "./GoalCard/GoalCard";
import GoalsHeader from "./GoalsHeader/GoalsHeader";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import CompletedGoalsAccordion from "./CompletedGoalsAccordion/CompletedGoalsAccordion";
import GoalDetails from "./GoalDetails/GoalDetails";
import DimmedText from "~/components/core/Text/DimmedText/DimmedText";
import { useTranslation } from "react-i18next";
import { InfoIcon } from "lucide-react";
import { useGoalsQuery } from "~/hooks/queries/useGoalsQuery";

const Goals = (): React.ReactNode => {
  const [includeInterest, { toggle: toggleIncludeInterest }] = useDisclosure();
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure();

  const { t } = useTranslation();
  const goalsQuery = useGoalsQuery({ includeInterest });

  const [selectedGoal, setSelectedGoal] = React.useState<IGoalResponse | null>(
    null,
  );

  React.useEffect(() => {
    if (goalsQuery.isError) {
      notifications.show({
        color: "var(--button-color-destructive)",
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

  return (
    <Stack w="100%" maw={1400}>
      <GoalDetails
        goal={selectedGoal}
        isOpen={isDetailsOpen}
        doClose={closeDetails}
      />
      <GoalsHeader
        includeInterest={includeInterest}
        toggleIncludeInterest={toggleIncludeInterest}
      />
      <Stack gap="0.5rem">
        {goalsQuery.isPending ? (
          <Skeleton h={100} w="100%" radius="lg" />
        ) : activeGoals.length === 0 ? (
          <Group justify="center" align="center" gap="0.5rem">
            <InfoIcon size={20} color="var(--base-color-text-dimmed)" />
            <DimmedText size="sm">{t("no_goals")}</DimmedText>
          </Group>
        ) : (
          activeGoals.map((goal: IGoalResponse) => (
            <GoalCard
              key={goal.id}
              goal={goal}
              includeInterest={includeInterest}
              openGoalDetails={openGoalDetails}
            />
          ))
        )}
      </Stack>
      {completedGoals.length > 0 && (
        <CompletedGoalsAccordion compeltedGoals={completedGoals} />
      )}
    </Stack>
  );
};

export default Goals;
