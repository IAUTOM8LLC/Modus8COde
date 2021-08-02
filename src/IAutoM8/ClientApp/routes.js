import React from "react";
import { Route, Switch } from "react-router-dom";

import { NotificationsCenter } from "@components";

import withRoles from "./infrastructure/HOC/withRoles";
import {
    AppWithAuth,
    Login,
    Signup,
    Projects,
    Formulas,
    Users,
    KanbanEditor,
    ForgotPassword,
    ForgotPasswordChange,
    ConfirmOwnerEmail,
    ConfirmEmail,
    CopyFormula,
    TaskChainingEditor,
    FormulaChainingEditor,
    Profile,
    Profiles,
    Skills,
    Clients,
    ProjectTasksFeed,
    TasksCalendarView,
    ProjectTaskLayout,
    //IntercomManager,
    Home,
    Credits,
    Balance,
    Terms,
    HomeList,
    Privacy,
    StartWork,
    Notifications,
    Teams,
    SuperUser,
    Payment
} from "./containers";

// Role guards
const allowManagers = withRoles(["Owner", "Manager"]);
const allowVendor = withRoles(["Vendor","Company","CompanyWorker"]);
//const allowVendor = withRoles(["Vendor"]);
const allowAdminManagers = withRoles(["Owner", "Manager", "Admin"]);
const allowAdmin = withRoles(["Admin"]);

// Wraps Component into layout component
const LayoutRoute = ({ layout: Layout, component: Component, ...rest }) => (
    <Route
        {...rest}
        render={(props) => (
            <Layout {...props}>
                <Component />
            </Layout>
        )}
    />
);

const ProjectSubrouter = ({ match }) => (
    <Switch>
        <LayoutRoute
            exact
            path={`${match.path}`}
            component={KanbanEditor}
            layout={ProjectTaskLayout}
        />
        <LayoutRoute
            path={`${match.path}/listView`}
            component={HomeList}
            layout={ProjectTaskLayout}
        />
        <LayoutRoute
            path={`${match.path}/calendar`}
            component={TasksCalendarView}
            layout={ProjectTaskLayout}
        />
        <LayoutRoute
            path={`${match.path}/editor`}
            component={allowManagers(TaskChainingEditor)}
            layout={ProjectTaskLayout}
        />
        <LayoutRoute
            path={`${match.path}/feed`}
            component={allowManagers(ProjectTasksFeed)}
            layout={ProjectTaskLayout}
        />
    </Switch>
);

const Router = () => (
    <div>
        <Switch>
            <Route path="/login" component={Login} />
            <Route path="/signup" component={Signup} />
            <Route path="/terms-conditions" component={Terms} />
            <Route path="/privacy-policy" component={Privacy} />
            <Route
                path="/vendor-signup"
                render={(props) => <Signup {...props} vendor />}
            />
            <Route
                path="/cvendor-signup"
                render={(props) => <Signup {...props} cVendor={true} />}
            />
            <Route path="/forgot-password" component={ForgotPassword} />
            <Route
                path="/forgot-password-change"
                component={ForgotPasswordChange}
            />
            <Route path="/confirm-owner-email" component={ConfirmOwnerEmail} />
            <Route path="/confirm-email" component={ConfirmEmail} />

            <AppWithAuth>
                <Switch>
                    {/* Home route */}
                    <Route exact path="/" component={Home} />

                    {/* Routes */}
                    <Route
                        path="/formulas/share/:formulaId"
                        component={allowManagers(CopyFormula)}
                    />
                    <Route
                        path="/formulas/:formulaId"
                        component={allowAdminManagers(FormulaChainingEditor)}
                    />
                    <Route
                        path="/formulas"
                        component={allowAdminManagers(Formulas)}
                    />
                    <Route
                        path="/notification/project-task/:notificationId"
                        render={(props) => (
                            <ProjectTaskLayout
                                {...props}
                                projectTaskNotification
                            >
                                {/* <KanbanEditor /> */}
                                <HomeList />
                            </ProjectTaskLayout>
                        )}
                    />
                    <Route
                        path="/notification/formula-task/:notificationId"
                        render={(props) => (
                            <ProjectTaskLayout
                                {...props}
                                formulaTaskNotification
                            >
                                {/* <KanbanEditor /> */}
                                <HomeList />
                            </ProjectTaskLayout>
                        )}
                    />
                    <Route
                        path="/accept-transer/:transferRequestId"
                        render={(props) => (
                            <ProjectTaskLayout {...props} transferRequest>
                                <KanbanEditor />
                            </ProjectTaskLayout>
                        )}
                    />
                    <Route
                        path="/projects/:projectId/task/:taskId/start-work"
                        component={StartWork}
                    />
                    <Route
                        path="/projects/:projectId/task/:taskId/:tab(info|comment)"
                        render={(props) => (
                            <ProjectTaskLayout {...props}>
                                <KanbanEditor />
                            </ProjectTaskLayout>
                        )}
                    />
                    <Route
                        exact
                        path="/projects/:projectId/task/:taskId"
                        render={(props) => (
                            <ProjectTaskLayout {...props}>
                                <KanbanEditor />
                            </ProjectTaskLayout>
                        )}
                    />
                    <Route
                        path="/projects/:projectId"
                        component={ProjectSubrouter}
                    />
                    <Route path="/projects" component={Projects} />
                    <Route path="/notifications" component={Notifications} />
                    <Route path="/users" component={allowManagers(Users)} />
                    <Route path="/profile" component={Profile} />
                    <Route path="/profiles" component={Profiles} />
                    <Route path="/skills" component={allowAdminManagers(Skills)} />
                    <Route path="/clients" component={allowManagers(Clients)} />
                    <Route path="/credits" component={allowManagers(Credits)} />
                    <Route path="/balance" component={allowVendor(Balance)} />
                    <Route path="/teams" component={allowAdminManagers(Teams)} />
                    <Route path="/admin" component={allowAdmin(SuperUser)} />
                    <Route path="/payment" component={allowAdmin(Payment)} />
                    {/* Catch all route 404 */}
                    <Route component={Home} />
                </Switch>
            </AppWithAuth>
        </Switch>

        {/* Replaced with new ChatBot in the Views/Index.cshtml page, IAutoM8 project */}
        {/* <IntercomManager /> */}
        <NotificationsCenter />
    </div>
);

export default Router;
