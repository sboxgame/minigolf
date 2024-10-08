import os

def create_vmdl_from_fbx(folder_path):
    template = """
    <!-- kv3 encoding:text:version{{e21c7f3c-8a33-41c5-9977-a76d3a32aa0d}} format:modeldoc29:version{{3cec427c-1b0e-4d48-a90a-0436f33a6041}} -->
    {{
        rootNode = 
        {{
            _class = "RootNode"
            children = 
            [
                {{
                    _class = "MaterialGroupList"
                    children = 
                    [
                        {{
                            _class = "DefaultMaterialGroup"
                            remaps = [  ]
                            use_global_default = false
                            global_default_material = ""
                        }},
                    ]
                }},
                {{
                    _class = "PhysicsShapeList"
                    children = 
                    [
                        {{
                            _class = "PhysicsMeshFile"
                            parent_bone = ""
                            surface_prop = "default"
                            collision_tags = "solid"
                            recenter_on_parent_bone = false
                            offset_origin = [ 0.0, 0.0, 0.0 ]
                            offset_angles = [ 0.0, 0.0, 0.0 ]
                            align_origin_x_type = "None"
                            align_origin_y_type = "None"
                            align_origin_z_type = "None"
                            filename = "{fbx_file}"
                            import_scale = 1.28
                            maxMeshVertices = 0
                            qemError = 0.0
                            import_filter = 
                            {{
                                exclude_by_default = false
                                exception_list = [  ]
                            }}
                        }},
                    ]
                }},
                {{
                    _class = "RenderMeshList"
                    children = 
                    [
                        {{
                            _class = "RenderMeshFile"
                            filename = "{fbx_file}"
                            import_translation = [ 0.0, 0.0, 0.0 ]
                            import_rotation = [ 0.0, 0.0, 0.0 ]
                            import_scale = 1.28
                            align_origin_x_type = "None"
                            align_origin_y_type = "None"
                            align_origin_z_type = "None"
                            parent_bone = ""
                            import_filter = 
                            {{
                                exclude_by_default = true
                                exception_list = 
                                [
                                    "Scene",
                                ]
                            }}
                        }},
                    ]
                }},
            ]
        }}
    }}
    """
    for file in os.listdir(folder_path):
        if file.endswith(".fbx"):
            fbx_name = os.path.splitext(file)[0]
            full_fbx_file_path = os.path.join(folder_path, file).replace('\\', '/')  # Full path with forward slashes
            models_index = full_fbx_file_path.find('/models/')  # Find the index of '/models/'
            if models_index != -1:
                fbx_file_path = full_fbx_file_path[models_index+1:]  # Extract from 'models/' onwards
            else:
                fbx_file_path = full_fbx_file_path  # Use the full path if 'models/' is not found

            vmdl_content = template.format(fbx_file=fbx_file_path, fbx_name=fbx_name)
            vmdl_filename = fbx_name + '.vmdl'
            with open(os.path.join(folder_path, vmdl_filename), 'w') as vmdl_file:
                vmdl_file.write(vmdl_content)
            print(f"Created {vmdl_filename}")

# Usage
folder_path = os.path.dirname(os.path.realpath(__file__))
create_vmdl_from_fbx(folder_path)
