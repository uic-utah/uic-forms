#!/usr/bin/env python
# * coding: utf8 *
'''
CreateVersionedViews.py
A module that creates the views the cli tool will use
'''

from os.path import join

import arcpy

arcpy.env.workspace = sde_file_path = 'C:\\Projects\\GitHub\\uic-7520\\database\\stage.sde'

table_to_view = {
    'UICAuthorization': 'Permit_view',
    'UICAuthorizationAction': 'Action_view',
    'UICWell': 'Well_view',
    'UICEnforcement': 'Enforcement_view',
    'UICViolation': 'Violation_view',
    'UICInspection': 'Inspection_view',
    'UICMIT': 'Mit_view'
}

#: if you are missing views this is how to create them
# tables = arcpy.ListTables() + arcpy.ListFeatureClasses() + arcpy.ListDatasets()
# for table in tables:
#     print('unregistering {}'.format(table))
#     arcpy.management.UnregisterAsVersioned(table, keep_edit='NO_KEEP_EDIT', compress_default='COMPRESS_DEFAULT')
#
#     print('registering {}'.format(table))
#     arcpy.management.RegisterAsVersioned(table)

print('creating versioned views')

for table, view in table_to_view.items():
    view_location = join(sde_file_path, view)

    if arcpy.Exists(view_location):
        print('{} exists. skipping...'.format(view))
        continue

    print('creating {}...'.format(view))
    table = join(sde_file_path, table)
    arcpy.management.CreateVersionedView(table, view)

print('finished')
