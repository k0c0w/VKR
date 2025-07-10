import { Link, LinkProps, useMatch, useResolvedPath } from 'react-router-dom';
import { useTheme } from '@mui/material/styles';

export function NavBarLink({activeOnExactRoute, ...props}:{activeOnExactRoute?: boolean} & LinkProps) {
  const theme = useTheme();
  const resolvedPath = useResolvedPath(props.to);
  const isActive = useMatch({ path: resolvedPath.pathname, end: activeOnExactRoute ?? true });

  const linkStyle = {
    color: isActive ? theme.palette.primary.main : theme.palette.text.primary,
    textDecoration: 'none',
    padding: '8px 16px',
    fontWeight: isActive ? 600 : 400,
    transition: 'color 0.3s, font-weight 0.3s',
  };

  return <Link {...props} style={{ ...linkStyle, ...props.style }} />;
}