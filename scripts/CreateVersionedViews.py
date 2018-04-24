#!/usr/bin/env python
# * coding: utf8 *
'''
CreateVersionedViews.py
A module that creates the views the cli tool will use
'''

from os.path import join

import arcpy

sde_file_path = 'C:\\Projects\\GitHub\\uic-7520\\database\\stage.sde'

table_to_view = {
  'UICAuthorization': 'Permit_view',
  'UICAuthorizationAction': 'Action_view',
  'UICWell': 'Well_view',
  'UICViolation': 'Violation_view',
  'UICViolationToEnforcement': 'ViolationEnforcement_lookup',
}

print('creating versioned views')

for table, view in table_to_view.items():
    view_location = join(sde_file_path, view)

    if arcpy.Exists(view_location):
        print('{} exists. skipping...'.format(view))
        continue

    print('creating {}...'.format(view))
    arcpy.management.CreateVersionedView(join(sde_file_path, table), view)

print('finished')
