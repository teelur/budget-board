import {
  Text,
  MantineColorScheme,
  NativeSelect,
  useMantineColorScheme,
} from "@mantine/core";

const DarkModeToggle = () => {
  const darkModeOptions = [
    { label: "Auto", value: "auto" },
    { label: "Light", value: "light" },
    { label: "Dark", value: "dark" },
  ];
  const { colorScheme, setColorScheme } = useMantineColorScheme();

  return (
    <NativeSelect
      data={darkModeOptions}
      label={
        <Text fw={600} size="sm">
          Appearance Mode
        </Text>
      }
      value={colorScheme}
      onChange={(event) =>
        setColorScheme(event.currentTarget.value as MantineColorScheme)
      }
    />
  );
};

export default DarkModeToggle;
