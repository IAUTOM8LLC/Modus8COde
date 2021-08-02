export function onDeleteOutsorsers(userOptions, skillMaps, skillId, userIds) {

    let updatedUserOptions = { ...userOptions };
    let updatedSkillMaps = [...skillMaps];
    if (updatedSkillMaps.find(t => t.skillId === skillId)
        .userIds.includes("outsource") && !userIds.includes("outsource")) {
        updatedUserOptions[skillId].users = userOptions[skillId].users
            .filter(t => t.key !== "outsource");
    }

    updatedSkillMaps.find(t => t.skillId === skillId).userIds = userIds;

    return {
        userOptions: updatedUserOptions,
        skillMaps: updatedSkillMaps
    };
}
