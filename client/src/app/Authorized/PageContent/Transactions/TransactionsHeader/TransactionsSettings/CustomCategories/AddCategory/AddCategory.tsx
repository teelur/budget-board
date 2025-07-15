import { AuthContext } from "~/components/AuthProvider/AuthProvider";
import CategorySelect from "~/components/CategorySelect";
import { translateAxiosError } from "~/helpers/requests";
import {
  Button,
  Card,
  LoadingOverlay,
  Switch,
  Stack,
  Text,
  TextInput,
  Group,
} from "@mantine/core";
import { isNotEmpty, useForm } from "@mantine/form";
import { notifications } from "@mantine/notifications";
import { ICategory, ICategoryCreateRequest } from "~/models/category";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import React from "react";

interface AddCategoryProps {
  categories: ICategory[];
}

interface FormValues {
  name: string;
  parent: string;
}

const AddCategory = (props: AddCategoryProps): React.ReactNode => {
  const [isChildCategory, setIsChildCategory] = React.useState(false);

  const { request } = React.useContext<any>(AuthContext);

  const queryClient = useQueryClient();
  const doAddCategory = useMutation({
    mutationFn: async (category: ICategoryCreateRequest) =>
      await request({
        url: "/api/transactionCategory",
        method: "POST",
        data: category,
      }),
    onSuccess: async () => {
      queryClient.invalidateQueries({ queryKey: ["transactionCategories"] });
      notifications.show({ color: "green", message: "Category added!" });
    },
    onError: (error: AxiosError) =>
      notifications.show({ color: "red", message: translateAxiosError(error) }),
  });

  const form = useForm({
    mode: "controlled",
    initialValues: { name: "", parent: "" },
    validate: {
      name: isNotEmpty("Name is required"),
    },
  });

  const parentCategories = props.categories.filter(
    (category) => category.parent?.length === 0
  );

  const handleSubmit = (values: FormValues) => {
    doAddCategory.mutate({
      value: values.name,
      parent: values.parent,
    });
    form.reset();
  };

  return (
    <Card withBorder shadow="xs">
      <LoadingOverlay visible={doAddCategory.isPending} />
      <form style={{ width: "100%" }} onSubmit={form.onSubmit(handleSubmit)}>
        <Stack>
          <TextInput
            {...form.getInputProps("name")}
            key={form.key("name")}
            label="Category Name"
            w="100%"
          />
          <Stack gap="0.5rem">
            <Text size="0.875rem">Category Type</Text>
            <Group gap="0.5rem">
              <Text size="0.875rem">Parent</Text>
              <Switch
                checked={isChildCategory}
                onChange={(event) =>
                  setIsChildCategory(event.currentTarget.checked)
                }
                size="md"
              />
              <Text size="0.875rem">Child</Text>
            </Group>
          </Stack>
          {isChildCategory && (
            <Stack gap="0.25rem">
              <Text size="0.875rem">Parent Category</Text>
              <CategorySelect
                w="100%"
                categories={parentCategories}
                value={form.getValues().parent}
                onChange={(value) => form.setFieldValue("parent", value)}
                withinPortal
              />
            </Stack>
          )}
          <Button w="100%" type="submit">
            Add Category
          </Button>
        </Stack>
      </form>
    </Card>
  );
};

export default AddCategory;
