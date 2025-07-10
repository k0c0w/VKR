import { BASE_URL } from "@app/config/env";
import { User } from "@entities/user/models/User";
import {createApi, fetchBaseQuery } from "@reduxjs/toolkit/query/react"
import { IUserList } from "../models/QueryUsers";
import { IRemoveUserArgs, IRemoveUserResult } from "../models/RemoveUser";
import { emit } from "process";

function mapRole(role: string) {
    if (role === 'editor') {
        return 2;
    }
    if (role === 'moderator') {
        return 3;
    }
    if (role === 'root') {
        return 4;
    }

    return 1;
}

const userManagementApi = createApi({
    reducerPath: "userManagementApi",
    baseQuery: fetchBaseQuery ({
        baseUrl: `${BASE_URL}/users`,
        credentials: 'include'
    }),
    tagTypes: ['users-list'],
    endpoints: (build) => ({
        getUserList: build.query<IUserList, void>({
            query: () => ({
                url: '/',
                method: 'GET'
            }),
            providesTags: ['users-list']
        }),
        changeUserRoles: build.mutation<User, User>({
            query: ({email, roles}) => ({
                url: `/${email}`,
                method: 'PUT',
                body: roles.map(x => mapRole(x))
            }),
            invalidatesTags: ['users-list']
        }),
        addUserToSystem: build.mutation<User, User>({
            query: (user) => ({
                url: `/`,
                method: 'POST',
                body: {
                    email: user.email,
                    roles: user.roles.map(x => mapRole(x))
                }
            }),
            invalidatesTags: ['users-list']
        }),
        removeUserFromSystem: build.mutation<IRemoveUserResult, IRemoveUserArgs>({
            query: ({email}) => ({
                url: `/${email}`,
                method: 'DELETE'
            }),
            invalidatesTags: ['users-list']
        })
    }) 
});

export {userManagementApi};