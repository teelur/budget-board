import { useAuth } from "~/providers/AuthProvider/AuthProvider";
import { Skeleton, Stack } from "@mantine/core";
import { useDidUpdate, useDisclosure } from "@mantine/hooks";
import { IGoalResponse } from "~/models/goal";
import { useQuery } from "@tanstack/react-query";
import { AxiosError, AxiosResponse } from "axios";
import React from "react";
import GoalCard from "./GoalCard/GoalCard";
import GoalsHeader from "./GoalsHeader/GoalsHeader";
import { notifications } from "@mantine/notifications";
import { translateAxiosError } from "~/helpers/requests";
import CompletedGoalsAccordion from "./CompletedGoalsAccordion/CompletedGoalsAccordion";
import GoalDetails from "./GoalDetails/GoalDetails";

const Goals = (): React.ReactNode => {
  const [includeInterest, { toggle: toggleIncludeInterest }] = useDisclosure();
  const [isDetailsOpen, { open: openDetails, close: closeDetails }] =
    useDisclosure();

  const [selectedGoal, setSelectedGoal] = React.useState<IGoalResponse | null>(
    null,
  );

  const { request } = useAuth();

  const goalsQuery = useQuery({
    queryKey: ["goals", { includeInterest }],
    queryFn: async (): Promise<IGoalResponse[]> => {
      const res: AxiosResponse = await request({
        url: "/api/goal",
        method: "GET",
        params: { includeInterest },
      });

      if (res.status === 200) {
        return res.data as IGoalResponse[];
      }

      return [];
    },
  });

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
