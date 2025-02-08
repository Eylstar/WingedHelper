local entities = require("entities")

local winged_adder = {}

winged_adder.name = "WingedHelper/WingedAdder"
winged_adder.depth = -100000

local outlineColor = { 0.737, 0.941, 0.960, 0.75 }
local fillColor = { 0.537, 0.698, 0.756, 0.45 }


local getAllSids = function ()
    return {}
end

if entities and entities.registeredEntities then
    getAllSids = function ()
        local ret = {}
        for k,v in pairs(entities.registeredEntities) do
            table.insert(ret, k)
        end
        table.sort(ret)
        return ret
    end
end


winged_adder.fieldInformation = function(entity) return {
    selectedTypes = {
        fieldType = "list",
        elementSeparator = ",",
        elementDefault = "",
        elementOptions = {
            options = getAllSids(),
            searchable = true,
        },
    },
    direction = {
        options = { "Up", "Down", "Left", "Right" },
        editable = false,
    },
    leftWingTint = {
        fieldType = "color",
        elementDefault = "FFFFFF",
    },
    rightWingTint = {
        fieldType = "color",
        elementDefault = "FFFFFF",
    },
} end

winged_adder.fieldOrder = function(entity) return{ 
    "x", "y", "width", "height", "leftWingXOffset", "rightWingXOffset", "leftWingYOffset", "rightWingYOffset","moveDelay", 
    "flySpeed", "direction", "selectedTypes", "leftWingTint", "rightWingTint", "blacklist", "inAreaRange", "actorsOnly", "collidablesOnly", "isHeavyWings", "allowInteractions", "disableCollisions", "rainbowWings",
}end



winged_adder.placements ={
    name = "WingedAdder",
    data = {
        width = 16,
        height = 16,
        selectedTypes = "",
        direction = "Up",
        leftWingTint = "FFFFFF",
        rightWingTint = "FFFFFF",
        blacklist = false,
        inAreaRange = true,
        actorsOnly = false,
        collidablesOnly = true,
        isHeavyWings = false,
        allowInteractions = true,
        disableCollisions = false,
        rainbowWings = false,
        moveDelay = 1.0,
        flySpeed = 25.0,
        leftWingXOffset = 0,
        leftWingYOffset = 0,
        rightWingXOffset = 0,
        rightWingYOffset = 0,
    },
}


winged_adder.fillColor = fillColor
winged_adder.borderColor = outlineColor


return winged_adder
