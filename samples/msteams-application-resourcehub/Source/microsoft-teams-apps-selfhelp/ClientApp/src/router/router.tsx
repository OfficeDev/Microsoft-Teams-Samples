// <copyright file="router.tsx" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>
import * as React from "react";
import { BrowserRouter, Route, Routes } from "react-router-dom";
import SignInPage from "../components/signin/sign-in";
import SignInSimpleStart from "../components/signin/sign-in-start";
import SignInSimpleEnd from "../components/signin/sign-in-end";
import UserDashboard from "../components/user-dashboard/user-dashboard";
import UserFeedback from "../components/user-feedback/user-feedback";
import ViewImageContent from "../components/view-image-content/view-image-content";
import ViewVideoContent from "../components/view-video-content/view-video-content";
import ViewContentShare from "../components/view-content-share/view-content-share";
import CarouselCard from "../components/user-dashboard/carousel-card";
import TrendingTopicCard from "../components/user-dashboard/trending-topic-card";
import AdminDashboard from "../components/admin-dashboard/admin-dashboard";
import AddNewArticle from "../components/add-new-article/add-new-article";
import EditArticle from "../components/add-new-article/edit-article";
import DeleteArticle from "../components/add-new-article/delete-article-dialog";
import SearchResult from "../components/search-result/search-result";
import ErrorPage from "../components/error-page";
import NotificationPreview from "../components/notification-preview/notification-preview";
import { NotificationProvider } from "../providers/notification-provider";
export const AppRoute: React.FunctionComponent<{}> = () => {
    return (
        <React.Fragment>
            <BrowserRouter>
                <Routes>
                    <Route path="/signin" element={SignInPage} />
                    <Route path="/signin-simple-start" element={SignInSimpleStart} />
                    <Route path="/signin-simple-end" element={<SignInSimpleEnd />} />
                    <Route path="/user-dashboard" element={<UserDashboard />} />
                    <Route path="/error-page" element={<ErrorPage />} />
                    <Route path="/carousel-card" element={<CarouselCard />} />
                    <Route path="/trending-topic-card" element={<TrendingTopicCard />} />
                    <Route path="/user-feedback" element={<UserFeedback />} />
                    <Route path="/view-image-content" element={<ViewImageContent />} />
                    <Route path="/view-video-content" element={<ViewVideoContent />} />
                    <Route path="/view-content-share" element={<ViewContentShare />} />
                    <Route path="/admin-dashboard" element={<NotificationProvider><AdminDashboard /></NotificationProvider>} />
                    <Route path="/add-new-article" element={<AddNewArticle />} />
                    <Route path="/edit-article/:articleId" element={<EditArticle />} />
                    <Route path="/search-result" element={<SearchResult />} />
                    <Route path="/delete-article-dialog" element={<DeleteArticle />} />
                    <Route path="/notification-preview" element={<NotificationPreview />} />
                </Routes>
            </BrowserRouter>
        </React.Fragment>
    );
};