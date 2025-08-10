import { convertNumberToCurrency, getCurrencySymbol } from "~/helpers/currency";
import classes from "./EditableGoalTargetAmountCell.module.css";

import { Flex, NumberInput, Text } from "@mantine/core";
import { IGoalResponse, IGoalUpdateRequest } from "~/models/goal";
import React from "react";
import { getGoalTargetAmount } from "~/helpers/goals";
import { sumAccountsTotalBalance } from "~/helpers/accounts";
import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import { useQuery } from "@tanstack/react-query";
import { IUserSettings } from "~/models/userSettings";
import { AxiosResponse } from "axios";

interface EditableGoalTargetAmountCellProps {
  goal: IGoalResponse;
  isSelected: boolean;
  editCell: (newGoal: IGoalUpdateRequest) => void;
}

const EditableGoalTargetAmountCell = (
  props: EditableGoalTargetAmountCellProps
): React.ReactNode => {
  const [goalAmountValue, setGoalAmountValue] = React.useState<number | string>(
    props.goal.amount
  );

  const { request } = React.useContext<any>(AuthContext);

  const userSettingsQuery = useQuery({
    queryKey: ["userSettings"],
    queryFn: async (): Promise<IUserSettings | undefined> => {
      const res: AxiosResponse = await request({
        url: "/api/userSettings",
        method: "GET",
      });

      if (res.status === 200) {
        return res.data as IUserSettings;
      }

      return undefined;
    },
  });

  const onInputBlur = (): void => {
    if (goalAmountValue && goalAmountValue.toString().length > 0) {
      const newGoal: IGoalUpdateRequest = {
        ...props.goal,
        amount: goalAmountValue as number,
      };
      if (props.editCell != null) {
        props.editCell(newGoal);
      }
    } else {
      setGoalAmountValue(props.goal.amount.toFixed(0));
    }
  };

  return (
    <Flex className={classes.container}>
      {userSettingsQuery.isPending ? null : (
        <Text size="lg" fw={600}>
          {convertNumberToCurrency(
            sumAccountsTotalBalance(props.goal.accounts) -
              props.goal.initialAmount,
            false,
            userSettingsQuery.data?.currency ?? "USD"
          )}
        </Text>
      )}
      <Text size="md" fw={600}>
        of
      </Text>
      {props.isSelected && props.goal.amount !== 0 ? (
        <Flex
          onClick={(e) => {
            e.stopPropagation();
          }}
        >
          <NumberInput
            maw={100}
            min={0}
            prefix={getCurrencySymbol(userSettingsQuery.data?.currency)}
            thousandSeparator=","
            onChange={setGoalAmountValue}
            onBlur={onInputBlur}
            value={goalAmountValue}
          />
        </Flex>
      ) : userSettingsQuery.isPending ? null : (
        <Text size="lg" fw={600}>
          {convertNumberToCurrency(
            getGoalTargetAmount(props.goal.amount, props.goal.initialAmount),
            false,
            userSettingsQuery.data?.currency ?? "USD"
          )}
        </Text>
      )}
    </Flex>
  );
};

export default EditableGoalTargetAmountCell;
