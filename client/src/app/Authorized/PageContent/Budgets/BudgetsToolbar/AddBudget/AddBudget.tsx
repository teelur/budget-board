import classes from "./AddBudget.module.css";

import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { translateAxiosError } from "~/helpers/requests";
import {
  ActionIcon,
  Button,
  LoadingOverlay,
  NumberInput,
  Popover,
  Stack,
} from "@mantine/core";
import { isNotEmpty, useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { IBudgetCreateRequest } from "~/models/budget";
import { ICategory } from "~/models/category";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import { PlusIcon, SendIcon } from "lucide-react";
import React from "react";

interface AddBudgetProps {
  date: Date;
  categories: ICategory[];
}

const AddBudget = (props: AddBudgetProps): React.ReactNode => {
  interface FormValues {
    category: string;
    limit: string | number;
  }
  const form = useForm<FormValues>({
    mode: "uncontrolled",
    initialValues: {
      category: "",
      limit: "",
    },

    validate: {
      category: isNotEmpty("Category is required"),
    },
  });

  const { request } = React.useContext<any>(AuthContext);
  const queryClient = useQueryClient();
  const doCreateBudget = useMutation({
    mutationFn: async (newBudget: IBudgetCreateRequest[]) =>
      await request({
        url: "/api/budget",
        method: "POST",
        data: newBudget,
      }),
    onSuccess: async () => {
      await queryClient.invalidateQueries({ queryKey: ["budgets"] });
      notifications.show({
        message: "Budget added successfully.",
        color: "green",
      });
    },
    onError: (error: AxiosError) => {
      notifications.show({
        message: translateAxiosError(error),
        color: "red",
      });
    },
  });

  const submitCreateBudget = (values: FormValues) => {
    doCreateBudget.mutate([
      {
        date: props.date,
        category: values.category,
        limit: values.limit === "" ? 0 : (values.limit as number),
      },
    ]);
  };

  return (
    <Popover>
      <Popover.Target>
        <Button>
          <PlusIcon />
        </Button>
      </Popover.Target>
      <Popover.Dropdown className={classes.root}>
        <LoadingOverlay visible={doCreateBudget.isPending} />
        <form
          className={classes.formContainer}
          onSubmit={form.onSubmit(submitCreateBudget)}
        >
          <Stack gap="sm">
            <CategorySelect
              value={form.getValues().category}
              onChange={(val) => form.setFieldValue("category", val)}
              key={form.key("category")}
              label="Category"
              categories={props.categories}
            />
            <NumberInput
              {...form.getInputProps("limit")}
              key={form.key("limit")}
              placeholder="Limit"
              w="100%"
              prefix="$"
              min={0}
              decimalScale={2}
              thousandSeparator=","
            />
          </Stack>
          <Stack className={classes.submitContainer}>
            <ActionIcon className={classes.submitButton} type="submit">
              <SendIcon size={18} />
            </ActionIcon>
          </Stack>
        </form>
      </Popover.Dropdown>
    </Popover>
  );
};

export default AddBudget;
