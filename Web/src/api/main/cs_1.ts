import request from '/@/utils/request';
enum Api {
  Addcs_1 = '/api/cs_1/add',
  Deletecs_1 = '/api/cs_1/delete',
  Updatecs_1 = '/api/cs_1/update',
  Pagecs_1 = '/api/cs_1/page',
}

// 增加测试
export const addcs_1 = (params?: any) =>
	request({
		url: Api.Addcs_1,
		method: 'post',
		data: params,
	});

// 删除测试
export const deletecs_1 = (params?: any) => 
	request({
			url: Api.Deletecs_1,
			method: 'post',
			data: params,
		});

// 编辑测试
export const updatecs_1 = (params?: any) => 
	request({
			url: Api.Updatecs_1,
			method: 'post',
			data: params,
		});

// 分页查询测试
export const pagecs_1 = (params?: any) => 
	request({
			url: Api.Pagecs_1,
			method: 'post',
			data: params,
		});


