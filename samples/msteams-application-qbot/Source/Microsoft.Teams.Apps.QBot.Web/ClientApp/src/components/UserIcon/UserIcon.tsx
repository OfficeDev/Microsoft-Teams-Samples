import React from 'react';
import { User } from 'models';
import { useEffect } from 'react';
import { Avatar, AvatarProps } from '@fluentui/react-northstar';
import { useActionCreator } from 'actionCreators';

export type UserIconProps = {
  user: User;
} & Omit<AvatarProps, 'image' | 'name'>;

export function UserIcon({ user, ...rest }: UserIconProps): JSX.Element {
  const loadUser = useActionCreator((s) => s.user.loadUser);
  useEffect(() => {
    if (!user.iconUrl) {
      loadUser({
        userId: user.id,
      });
    }
  }, [loadUser, user.id, user.iconUrl]);
  return <Avatar image={user.iconUrl} name={user.name} {...rest} />;
}
