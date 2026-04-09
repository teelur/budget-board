import { Stack } from "@mantine/core";
import React from "react";
import EnableAutoCategorizer from "./EnableAutoCategorizer/EnableAutoCategorizer";
import AutoCategorizerMinimumProbability from "./AutoCategorizerMinimumProbability/AutoCategorizerMinimumProbability";
import TrainAutoCategorizerModal from "./TrainAutoCategorizerModal/TrainAutoCategorizerModal";

const AutoCategorizer = (): React.ReactNode => {
  return (
    <Stack gap="0.5rem">
      <EnableAutoCategorizer />
      <AutoCategorizerMinimumProbability />
      <TrainAutoCategorizerModal />
    </Stack>
  );
};

export default AutoCategorizer;
