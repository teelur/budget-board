import { useIsMobile } from '@/components/hooks/use-mobile';
import { SidebarTrigger } from '@/components/ui/sidebar';
import { MenuIcon } from 'lucide-react';

const Header = (): JSX.Element => {
  const isMobile = useIsMobile();
  return (
    <div className="flex w-full flex-row items-center gap-2">
      {isMobile && (
        <SidebarTrigger>
          <MenuIcon />
        </SidebarTrigger>
      )}
      <h2 className="grow text-3xl font-semibold tracking-tight">Budget Board</h2>
    </div>
  );
};

export default Header;
