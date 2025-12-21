import { Center, Loader } from "@mantine/core";
import React from "react";

const LoadingScreen = (): React.ReactNode => {
  return (
    <Center bg="var(--background-color-base)" h="100vh">
      <Loader size={100} />
    </Center>
  );
};

export default LoadingScreen;
