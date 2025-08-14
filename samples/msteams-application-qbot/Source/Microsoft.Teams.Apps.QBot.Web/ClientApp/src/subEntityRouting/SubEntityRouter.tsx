import { useTeamsContext } from 'components/TeamsProvider/hooks';
import React, { useEffect } from 'react';
import { usePushRelativePath } from 'hooks';
import { useDispatch } from 'react-redux';

type SubEntityNavigationPage = 'courseConfig';
const subEntityNavigationMappers: Record<
  SubEntityNavigationPage,
  (id: string) => string
> = {
  courseConfig: (id) => `/courses/${id}/configure/general`,
};

export function SubEntityRouter() {
  const teamsContext = useTeamsContext();
  const push = usePushRelativePath();
  const dispatch = useDispatch();
  useEffect(() => {
    if (!teamsContext.subEntityId) {
      return;
    }
    const subEntityId = teamsContext.subEntityId;
    const [page, id] = subEntityId.split(':/', 2);
    if (page in subEntityNavigationMappers) {
      const p = page as SubEntityNavigationPage;
      const nextPage = subEntityNavigationMappers[p](id);
      dispatch(push(nextPage));
    }
  }, [teamsContext, push, dispatch]);
  return <> </>;
}
