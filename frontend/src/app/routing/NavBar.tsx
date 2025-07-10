import { styled } from '@mui/material/styles';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Container from '@mui/material/Container';
import { useAppSelector } from '@shared/hooks/reduxTypedHooks';
import { Stack, Typography } from '@mui/material';
import { useMatch, useResolvedPath, useLocation, useNavigate } from 'react-router-dom';
import { Routes } from './routes';
import { NavBarLink } from './NavBarLink';
import LogoutIcon from '@mui/icons-material/Logout';
import IconButton from '@mui/material/IconButton';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';

const StyledToolbar = styled(Toolbar)(({ theme }) => ({
  display: 'flex',
  justifyContent: 'center',
  flexShrink: 0,
  borderRadius: 0,
  backdropFilter: 'blur(24px)',
  borderBottom: '1px solid',
  borderColor: theme.palette.divider,
  color: theme.palette.text.primary,
  boxShadow: theme.shadows[1],
  padding: '8px 0',
  width: '100%',
  maxWidth: 'none',
}));

export default function NavBar() {
  const currentUser = useAppSelector((s) => s.authSlice.currentUser);
  const authorized = currentUser !== undefined;
  const userIsModeratorOrRoot =
    currentUser !== undefined && (currentUser.roles?.includes('moderator') || currentUser.roles?.includes('root'));

  const navigate = useNavigate();
  const { pathname } = useLocation();
  const path = useResolvedPath(Routes.SignInRouteTemplate);
  const isLoginPath = useMatch({ path: path.pathname, end: true });

  if (isLoginPath) {
    return <></>;
  }

  const isSinglePath = pathname.split('/').filter(segment => segment).length === 1;
  const parentPath = pathname.split('/').slice(0, -1).join('/') || '/';

  return (
    <AppBar
      position="static"
      enableColorOnDark
      sx={{
        boxShadow: 0,
        bgcolor: 'transparent',
        backgroundImage: 'none',
        width: '100%',
        maxWidth: 'none',
      }}
    >
      <StyledToolbar variant="dense" disableGutters>
        <Container maxWidth="xl" sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <Box sx={{ display: { xs: 'none', md: 'flex' }, justifyContent: 'flex-start', gap: 2, alignItems: 'center' }}>
            {isSinglePath ? (
              <>
                <NavBarLink to={Routes.AvailablePlansRouteTemplate} activeOnExactRoute={false}>Планы</NavBarLink>
                {userIsModeratorOrRoot && (
                  <NavBarLink to={Routes.UserManagmentRouteTemplate}>Управление пользователями</NavBarLink>
                )}
              </>
            ) : (
              <IconButton onClick={() => navigate(parentPath)} color="inherit" aria-label="go back">
                <ArrowBackIcon />
              </IconButton>
            )}
          </Box>
          <Box sx={{ display: { xs: 'none', md: 'flex' }, gap: 1, justifyContent: 'flex-end', alignItems: 'center' }}>
            {authorized && (
              <Stack sx={{ flexDirection: 'column', alignItems: 'flex-end' }}>
                <NavBarLink to={Routes.SignOutRouteTemplate} style={{ padding: '8px 16px' }}>
                  <LogoutIcon />
                </NavBarLink>
                <Typography component="div" textAlign="center">{currentUser.email}</Typography>
              </Stack>
            )}
          </Box>
        </Container>
      </StyledToolbar>
    </AppBar>
  );
}